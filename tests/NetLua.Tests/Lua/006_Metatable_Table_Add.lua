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

assert(t1 + t2 == 1)
assert(t2 + t1 == 2)
assert(t2 + t2 == 2)