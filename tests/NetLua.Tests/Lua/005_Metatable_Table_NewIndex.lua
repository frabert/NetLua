local t = {}
local data = {};

setmetatable(t, {
__newindex = data
})

t.a = true

assert(data.a)