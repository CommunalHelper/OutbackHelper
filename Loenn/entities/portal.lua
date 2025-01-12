local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local portal = {}

portal.name = "outback/portal"
portal.depth = -10000
portal.justification = {0.5, 1.0}
portal.placements = {}

local colors = {
    ["Purple"] = {1.0, 0.3, 1.0},
    ["Blue"] = {0.3, 0.3, 1.0},
    ["Red"] = {1.0, 0.3, 0.3},
    ["Yellow"] = {1.0, 1.0, 0.3},
    ["Green"] = {0.3, 1.0, 0.3},
    ["Aqua"] = {0.0, 1.0, 1.0},
    ["Black"] = {0.0, 0.0, 0.0},
    ["Fuchsia"] = {1.0, 0.0, 1.0},
    ["Gray"] = {0.5, 0.5, 0.5},
    ["Lime"] = {0.0, 1.0, 0.0},
    ["Maroon"] = {0.5, 0.0, 0.0},
    ["Navy"] = {0.0, 0.0, 0.5},
    ["Olive"] = {0.5, 0.5, 0},
    ["Silver"] = {0.75, 0.75, 0.75},
    ["Teal"] = {0.0, 0.5, 0.5},
    ["White"] = {1.0, 1.0, 1.0}
}

local directions = {
    ["None"] = 0,
    ["Up"] = 1,
    ["Down"] = 2,
    ["Left"] = 3,
    ["Right"] = 4
}

local rotations = {
    ["Up"] = 0,
    ["Down"] = math.pi,
    ["Left"] = math.pi * 3 / 2,
    ["Right"] = math.pi / 2
}

portal.fieldInformation = {
    readyColor = {
        options = {"Purple", "Blue", "Red", "Yellow", "Green", "Aqua", "Black", "Fuchsia", "Gray", "Lime", "Maroon", "Navy", "Olive", "Silver", "Teal", "White"},
        editable = false
    },
    direction = {
        options = {"None", "Up", "Down", "Left", "Right"},
        editable = false
    }
}

for i, j in pairs(directions) do
    table.insert(portal.placements, {
        name = i,
        data = {
            ["readyColor"] = "Purple",
            direction = i,
            cooldownTimer = 0.0,
            fixRotationAngle = true,
            wallSpeedRetentionProtection = false,
        }
    })
end

portal.fieldOrder = {
    "x", "y",
    "direction",
    "readyColor",
    "cooldownTimer"
}

local nonDirectionalTexture = "objects/outback/portal/idle00"
local directionalTexture = "objects/outback/portal/directional00"

function portal.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local color = entity["readyColor"] or "Purple"
    local direction = entity["direction"] or "None"

    local sprite = drawableSprite.fromTexture(directionalTexture, entity)

    if direction == "None" then
        sprite = drawableSprite.fromTexture(nonDirectionalTexture, entity)
        sprite:setJustification(0.5, 0.5)
        sprite:setColor(colors[color])
    else
        local rotation = rotations[direction]
        sprite:setJustification(0.5, 0.5)
        sprite:setPosition(x - math.sin(rotation) * 1, y + math.cos(rotation))
        sprite:setColor(colors[color])
        sprite.rotation = rotation
    end

    return sprite
end

function portal.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local direction = entity["direction"] or "None"

    if direction == "None" then
        return utils.rectangle(x - 7, y - 7, 14, 14)
    elseif direction == "Up" then
        return utils.rectangle(x - 7, y, 14, 7)
    elseif direction == "Down" then
        return utils.rectangle(x - 7, y - 7, 14, 7)
    elseif direction == "Right" then
        return utils.rectangle(x - 7, y - 7, 7, 14)
    elseif direction == "Left" then
        return utils.rectangle(x, y - 7, 7, 14)
    end
end

function portal.rotate(room, entity, direction)
    local directionsLookup = {
        [0] = "Right",
        [1] = "Up",
        [2] = "Left",
        [3] = "Down",
        ["Right"] = 0,
        ["Up"] = 1,
        ["Left"] = 2,
        ["Down"] = 3,
    }

    local dir = entity.direction or "None"
    if dir == "None" then
        return false
    end
    dir = (directionsLookup[dir] - direction + 4) % 4
    entity.direction = directionsLookup[dir]
    return true
end
return portal