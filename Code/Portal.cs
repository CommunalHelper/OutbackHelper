using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.OutbackHelper {
    [Tracked(false)]
    public class Portal : Entity {
        private Level level {
            get {
                return (Level)base.Scene;
            }
        }

        public Portal(Vector2[] nodes, EntityData data, Vector2 offset) : base(data.Position + offset) {
            this.readyColor = (int)data.Enum<Portal.ReadyColors>("readyColor", Portal.ReadyColors.Purple);
            this.direction = (int)data.Enum<Portal.Directions>("direction", Portal.Directions.None);
            nodes = data.NodesOffset(offset);
            this.portalNodes = nodes;
            base.Depth = -9999;
            this.portal = base.Get<Sprite>();
            this.maxCooldown = data.Float("cooldownTimer", 0f);
            base.Add(this.portal = OutbackModule.SpriteBank.Create("portal"));
            base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, new Hitbox(30f, 30f, -15f, -15f)));
            this.portal.CenterOrigin();
            bool flag = this.direction == 0;
            if (flag) {
                this.portal.Play("idle", true, false);
                base.Collider = new Hitbox(12f, 12f, -6f, -6f);
            } else {
                this.portal.Play("directional", true, false);
                bool flag2 = this.direction != 0;
                if (flag2) {
                    this.Position -= this.directionsArray[this.direction - 1];
                }
                bool flag3 = this.direction == 1;
                if (flag3) {
                    base.Collider = new Hitbox(16f, 6f, -8f, 0.5f);
                } else {
                    bool flag4 = this.direction == 2;
                    if (flag4) {
                        base.Collider = new Hitbox(16f, 6f, -8f, -7.5f);
                    } else {
                        bool flag5 = this.direction == 3;
                        if (flag5) {
                            base.Collider = new Hitbox(8f, 16f, -1f, -8f);
                        } else {
                            bool flag6 = this.direction == 4;
                            if (flag6) {
                                base.Collider = new Hitbox(8f, 16f, -7f, -8f);
                            }
                        }
                    }
                }
            }
            base.Add(this.light = new VertexLight(Color.White, 1f, 16, 32));
            bool flag7 = this.direction != 0;
            if (flag7) {
                base.Add(new StaticMover {
                    SolidChecker = new Func<Solid, bool>(this.IsRiding)
                });
            }
            this.light.Visible = true;
        }


        public override void Added(Scene scene) {
            base.Added(scene);
            Level level = scene as Level;
            bool flag = level == null;
            if (!flag) {
                this.start = this.Position;
                this.startLevel = level.Session.Level;
                this.portal.OnFrameChange = new Action<string>(this.OnAnimate);
            }
        }


        public override void Awake(Scene scene) {
            base.Awake(scene);
            this.playerEntities = scene.Entities.OfType<Player>().ToList<Player>();
            if (playerEntities.Count == 0)
                return;
            this.playerEntity = this.playerEntities[0];
            bool flag = this.direction != 0;
            if (flag) {
                this.portal.Rotation = (float)this.directionInRadiansArray[this.direction - 1];
            }
            this.portals = this.level.Tracker.GetEntities<Portal>();
            this.portal.Color = this.readyColorsArray[this.readyColor];
            for (int i = 0; i < this.portals.Count; i++) {
                Portal portal = (Portal)this.portals[i];
                bool flag2 = portal.readyColor == this.readyColor && portal != this;
                if (flag2) {
                    this.otherPortal = portal;
                }
            }
        }


        private bool IsRiding(Solid solid) {
            bool result;
            switch (this.direction) {
                case 1:
                    result = base.CollideCheckOutside(solid, this.Position + Vector2.UnitY);
                    break;
                case 2:
                    result = base.CollideCheck(solid, this.Position - Vector2.UnitY * 10f);
                    break;
                case 3:
                    result = base.CollideCheckOutside(solid, this.Position + Vector2.UnitX * 10f);
                    break;
                case 4:
                    result = base.CollideCheckOutside(solid, this.Position - Vector2.UnitX * 10f);
                    break;
                default:
                    result = false;
                    break;
            }
            this.hasStaticMover = result;
            return result;
        }


        public override void Update() {
            //general cooldown
            this.cooldown -= Engine.DeltaTime;
            base.Collidable = this.cooldown < 0;

            //cooldown while the play is inside the portal
            bool flag = this.teleportInsideCooldown > 0f;
            if (flag) {
                bool flag2 = this.direction == 1;
                if (flag2) {
                    this.teleportInsideCooldown -= Engine.DeltaTime;
                }
                bool flag3 = !base.CollideCheck(this.playerEntity);
                if (flag3) {
                    this.teleportInsideCooldown = 0f;
                    bool flag4 = this.playerEntity.StateMachine.State == 11;
                    if (flag4) {
                        this.playerEntity.StateMachine.State = 0;
                    }
                }
            } else if(base.Collidable && this.portal.Color == cooldownColor) {
                this.portal.Color = this.readyColorsArray[this.readyColor];
                this.level.Session.SetFlag("portalOnCooldown" + readyColor.ToString(), false);
            }

            base.Update();
        }


        public void OnAnimate(string id) {
            int currentAnimationFrame = this.portal.CurrentAnimationFrame;
            bool flag = id == "teleport";
            if (flag) {
                bool flag2 = currentAnimationFrame == 5;
                if (flag2) {
                    bool flag3 = this.direction == 0;
                    if (flag3) {
                        this.portal.Play("idle", true, false);
                    } else {
                        this.portal.Play("directional", true, false);
                    }
                }
            }
        }


        private void OnPlayer(Player player) {
            bool flag = this.teleportInsideCooldown <= 0f && otherPortal != null;
            if (flag) {
                Portal portal = (Portal)this.otherPortal;
                bool flag2 = this.direction == 0;
                if (flag2) {
                    this.portal.Play("teleport", true, false);
                    portal.portal.Play("teleport", true, false);
                    Vector2 vec = this.otherPortal.Center + new Vector2(0f, 6f);
                    player.Position = vec.Round();
                }
                bool flag3 = portal.direction != 0;
                if (flag3) {
                    this.portals = this.level.Tracker.GetEntities<Portal>();
                    bool flag4 = !portal.Active;
                    if (flag4) {
                        for (int i = 0; i < this.portals.Count; i++) {
                            Portal portal2 = (Portal)this.portals[i];
                            bool flag5 = portal2.readyColor == this.readyColor && portal2 != this && portal2.Active;
                            if (flag5) {
                                this.otherPortal = portal2;
                            }
                        }
                    }
                    Vector2 vec2 = this.otherPortal.Center + new Vector2(0f, 6f) + this.directionsArray[portal.direction - 1] * 5f;
                    player.Position = vec2.Round();
                    float radians = (float)((double)(portal.portal.Rotation - this.portal.Rotation) + 3.141592653589793);
                    Vector2 vector = new Vector2((float)Math.Cos((double)this.portal.Rotation), -(float)Math.Sin((double)this.portal.Rotation));
                    List<Solid> list = base.Scene.CollideAll<Solid>(new Rectangle((int)portal.X, (int)portal.Y, (int)this.directionsArray[portal.direction - 1].X * 8, (int)this.directionsArray[portal.direction - 1].Y * 8));
                    List<SolidTiles> list2 = base.Scene.CollideAll<SolidTiles>(new Rectangle((int)portal.X, (int)portal.Y, (int)this.directionsArray[portal.direction - 1].X * 8, (int)this.directionsArray[portal.direction - 1].Y * 8));
                    foreach (Entity entity in base.Scene.Tracker.GetEntities<Platform>()) {
                        bool flag6 = entity is SolidTiles;
                        if (flag6) {
                            bool flag7 = base.Scene.CollideCheck(new Rectangle((int)portal.X, (int)portal.Y, (int)this.directionsArray[portal.direction - 1].X * 8, (int)this.directionsArray[portal.direction - 1].Y * 8), entity);
                            if (flag7) {
                                list2.Add((SolidTiles)entity);
                            }
                        }
                    }
                    bool flag8 = list.Count > 0 || list2.Count > 0;
                    if (flag8) {
                        player.Die(Vector2.Zero, false, true);
                    }
                    bool flag9 = this.direction == 1;
                    if (flag9) {
                        player.Speed.Y = Math.Max(player.Speed.Y, 150f);
                    }
                    int facingInt = (directionsArray[direction - 1].X == directionsArray[portal.direction - 1].X) ? -(int)player.Facing : (int)player.Facing;
                    player.Facing = (Facings)facingInt;
                    player.Speed = Vector2.Transform(player.Speed, Matrix.CreateRotationZ(radians));
                    bool flag10 = player.StateMachine.State == 2;
                    if (flag10) {
                        player.StateMachine.State = 11;
                    }
                    bool flag11 = this.direction == 1;
                    if (flag11) {
                        bool flag12 = player.StateMachine.State != 5;
                        if (flag12) {
                            player.Speed *= 1.5f;
                        }
                        bool flag13 = portal.direction == 3 || portal.direction == 4;
                        if (flag13) {
                            bool flag14 = player.StateMachine.State != 5;
                            if (flag14) {
                                player.Speed.Y = player.Speed.Y - 150f;
                            }
                        }
                    } else {
                        bool flag15 = this.direction == 2;
                        if (flag15) {
                            bool flag16 = player.StateMachine.State != 5;
                            if (flag16) {
                                player.Speed *= 1.5f;
                            }
                        }
                    }
                    bool flag17 = portal.direction == 1;
                    if (flag17) {
                        player.Speed.Y = Math.Min(player.Speed.Y, -150f);
                    }
                    bool flag18 = this.hasStaticMover && player.StateMachine.State != 5;
                    if (flag18) {
                        bool flag19 = base.Get<StaticMover>().Platform.GetType() != typeof(CassetteBlock);
                        if (flag19) {
                            player.Speed += base.Get<StaticMover>().Platform.LiftSpeed;
                        }
                    }
                }
                Audio.Play("event:/char/badeline/disappear", player.Position);
                this.level.Displacement.AddBurst(this.otherPortal.Position, 0.35f, 8f, 48f, 0.25f, null, null);
                this.level.Displacement.AddBurst(this.Position, 0.35f, 8f, 48f, 0.25f, null, null);
                this.level.Particles.Emit(Player.P_Split, 16, this.otherPortal.Center, Vector2.One * 6f);
                portal.teleportInsideCooldown = 0.5f;
                this.teleportInsideCooldown = 0.5f;
                portal.cooldown = portal.maxCooldown;
                this.cooldown = this.maxCooldown;
                portal.portal.Color = this.cooldownColor;
                this.portal.Color = this.cooldownColor;
                this.level.Session.SetFlag("portalOnCooldown" + readyColor.ToString(), true);
            }
        }


        private VertexLight light;


        private Vector2 start;


        private string startLevel;


        private Vector2[] portalNodes;


        public Sprite portal;


        private Entity otherPortal;


        private readonly float maxCooldown;


        private int readyColor;


        private Color[] readyColorsArray = new Color[]
        {
            new Color(1f, 0.3f, 1f),
            new Color(0.3f, 0.3f, 1f),
            new Color(1f, 0.3f, 0.3f),
            new Color(1f, 1f, 0.3f),
            new Color(0.3f, 1f, 0.3f),
            new Color(0.0f, 1.0f, 1.0f),
            new Color(0.0f, 0.0f, 0.0f),
            new Color(1.0f, 0.0f, 1.0f),
            new Color(0.5f, 0.5f, 0.5f),
            new Color(0.0f, 1.0f, 0.0f),
            new Color(0.5f, 0.0f, 0.0f),
            new Color(0.0f, 0.0f, 0.5f),
            new Color(0.5f, 0.5f, 0f),
            new Color(0.75f, 0.75f, 0.75f),
            new Color(0.0f, 0.5f, 0.5f),
            new Color(1.0f, 1.0f, 1.0f)
        };


        private Vector2[] directionsArray = new Vector2[]
        {
            new Vector2(0f, -1f),
            new Vector2(0f, 1f),
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f)
        };


        private double[] directionInRadiansArray = new double[]
        {
            0.0,
            3.141592653589793,
            -1.5707963267948966,
            1.5707963267948966
        };


        private Color cooldownColor = new Color(1f, 0.5f, 0.5f);


        public float teleportInsideCooldown;


        private float cooldown;


        private List<Entity> portals;


        private int direction;


        private List<Player> playerEntities;


        private Player playerEntity;


        private const float verticalMultiplier = 1.5f;


        private bool hasStaticMover = false;


        private enum ReadyColors {
            Purple,
            Blue,
            Red,
            Yellow,
            Green,
            Aqua,
            Black,
            Fuchsia,
            Gray,
            Lime,
            Maroon,
            Navy,
            Olive,
            Silver,
            Teal,
            White
        }


        private enum Directions {

            None,

            Up,

            Down,

            Left,

            Right
        }
    }
}