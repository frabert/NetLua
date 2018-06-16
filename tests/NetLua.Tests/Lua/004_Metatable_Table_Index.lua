local t = {}

setmetatable(t, {__index = {a = true}})

assert.True(t.a)