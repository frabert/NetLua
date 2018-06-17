function createtable(...)
	return {...}
end

local t = createtable("foo", "bar")
assert.Equal("foo", t[1])
assert.Equal("bar", t[2])

function createtable2(...)
	return {..., "baz"}
end

local t2 = createtable2("foo", "bar")
assert.Equal("foo", t2[1])
assert.Equal("baz", t2[2])

function ret(...)
	return ...
end

local t3 = {ret("foo", "bar")}
assert.Equal("foo", t3[1])
assert.Equal("bar", t3[2])