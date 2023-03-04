module Outback

using ..Ahorn, Maple

@mapdef Entity "outback/movingtouchswitch" MovingTouchSwitch(x::Integer, y::Integer)
@mapdef Entity "outback/portal" Portal(x::Integer, y::Integer)
@mapdef Entity "outback/timedtouchswitch" TimedTouchSwitch(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Touch Switch (Moving) (Outback)" => Ahorn.EntityPlacement(
        MovingTouchSwitch
    )
)

touchSwitchMovements = Dict{String, Integer}(
    "Slow" => 15,
    "Medium" => 10,
    "Fast" => 5
)

colors = Dict{String, Any}(
    "Purple" => (1.0, 0.3, 1.0, 1.0),
    "Blue" => (0.3, 0.3, 1.0, 1.0),
    "Red" => (1.0, 0.3, 0.3, 1.0),
    "Yellow" => (1.0, 1.0, 0.3, 1.0),
    "Green" => (0.3, 1.0, 0.3, 1.0),
    "Aqua" => (0.0, 1.0, 1.0, 1.0),
    "Black" => (0.0, 0.0, 0.0, 1.0),
    "Fuchsia" => (1.0, 0.0, 1.0, 1.0),
    "Gray" => (0.5, 0.5, 0.5, 1.0),
    "Lime" => (0.0, 1.0, 0.0, 1.0),
    "Maroon" => (0.5, 0.0, 0.0, 1.0),
    "Navy" => (0.0, 0.0, 0.5, 1.0),
    "Olive" => (0.5, 0.5, 0.0, 1.0),
    "Silver" => (0.75, 0.75, 0.75, 1.0),
    "Teal" => (0.0, 0.5, 0.5, 1.0),
    "White" => (1.0, 1.0, 1.0, 1.0)
)

portalDirections = Dict{String, Integer}(
    "None" => 0,
    "Up" => 1,
    "Down" => 2,
    "Left" => 3,
    "Right" => 4
)

rotations = Dict{Number, Number}(
    1 => 0,
    2 => pi,
    3 => pi * 3 / 2,
    4 => pi / 2
)

for (portalDirection, int) in portalDirections
    for (colorName, color) in colors
    const placements["Portal ($portalDirection, $colorName) (Outback)"] = Ahorn.EntityPlacement(
        Portal,
        "point",
        Dict{String, Any}(
            "readyColor" => colorName,
            "direction" => portalDirection
        )
    )
    end
end

Ahorn.editingOptions(entity::Portal) = Dict{String, Any}(
    "readyColor" => collect(keys(colors)),
    "direction" => collect(keys(portalDirections))
)

for (speedName, delayTime) in touchSwitchMovements
    const placements["Touch Switch (Timed, $speedName) (Outback)"] = Ahorn.EntityPlacement(
        TimedTouchSwitch,
        "point",
        Dict{String, Any}(
            "startDisappearTime" => delayTime
        )
    )
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::MovingTouchSwitch)
    px, py = Ahorn.position(entity)

    sprite = "collectables/outback/movingtouchswitch/container.png"

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.selection(entity::MovingTouchSwitch)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    sprite = "collectables/outback/movingtouchswitch/container.png"

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.selection(entity::Portal)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("objects/outback/portal/idle00.png", x, y, jx=0.5, jy=0.5)
end

function Ahorn.selection(entity::TimedTouchSwitch)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 7, y - 7, 14, 14)
end

Ahorn.nodeLimits(entity::MovingTouchSwitch) = 0, -1

borderMultiplier = (0.9, 0.9, 0.9, 1)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MovingTouchSwitch)
    Ahorn.drawSprite(ctx, "collectables/outback/movingtouchswitch/container.png", 0, 0)
    Ahorn.drawSprite(ctx, "collectables/outback/movingtouchswitch/icon00.png", 0, 0)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Portal)
    readyColor = get(entity.data, "readyColor", "Purple")
    if haskey(portalDirections, get(entity.data, "direction", "None"))
        direction = get(portalDirections, get(entity.data, "direction", "None"), 0)
        if direction != 0
            theta = rotations[direction] - pi / 2
    
            width = Int(get(entity.data, "width", 0))
            height = Int(get(entity.data, "height", 0))
    
            x, y = Ahorn.position(entity)
            cx, cy = floor(Int, width / 2) - 8 * (direction == 3), floor(Int, height / 2) - 8 * (direction == 1)

            if direction == 1
                Ahorn.drawSprite(ctx, "objects/outback/portal/directional00.png", 0, 1, tint = colors[readyColor], rot = theta + pi / 2)
            elseif direction == 2
                Ahorn.drawSprite(ctx, "objects/outback/portal/directional00.png", 14, 13, tint = colors[readyColor], rot = theta + pi / 2)
            elseif direction == 3
                Ahorn.drawSprite(ctx, "objects/outback/portal/directional00.png", 1, 13, tint = colors[readyColor], rot = theta + pi / 2)
            elseif direction == 4
                Ahorn.drawSprite(ctx, "objects/outback/portal/directional00.png", 13, 0, tint = colors[readyColor], rot = theta + pi / 2)
            end
        else
            Ahorn.drawSprite(ctx, "objects/outback/portal/idle00.png", 0, 0, tint = colors[readyColor])
        end
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimedTouchSwitch)
    timeLimit = get(entity.data, "startDisappearTime", "Slow")
    tintColor = (0.3, 1.0, 0.3, 1.0)

    if timeLimit <= 5
        tintColor = (1.0, 0.3, 0.3, 1.0)
    elseif timeLimit <= 10
        tintColor = (1.0, 1.0, 0.3, 1.0)
    end

    Ahorn.drawSprite(ctx, "collectables/outback/timedtouchswitch/cont.png", 0, 0, tint = tintColor)
    Ahorn.drawSprite(ctx, "collectables/outback/timedtouchswitch/idle00.png", 0, 0, tint = tintColor)
end

end