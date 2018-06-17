local t1 = {}
local t2 = {}

setmetatable(t1, {
    __add = function(l, r) 
        return 1
    end
})

setmetatable(t2, {
    __add = function(l, r) 
        return 2
    end
})

assert.Equal(1, t1 + t2)
assert.Equal(2, t2 + t1)
assert.Equal(2, t2 + t2)