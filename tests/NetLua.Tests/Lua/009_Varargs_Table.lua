function createtable(...)
	return {...}
end

local t = createtable("foo", "bar")
assert.Equal(t[1], "foo")
assert.Equal(t[2], "bar")

function createtable2(...)
	return {..., "baz"}
end

local t2 = createtable2("foo", "bar")
assert.Equal(t2[1], "foo")
assert.Equal(t2[2], "baz")

function ret(...)
	return ...
end

local t3 = {ret("foo", "bar")}
assert.Equal(t3[1], "foo")
assert.Equal(t3[2], "bar")