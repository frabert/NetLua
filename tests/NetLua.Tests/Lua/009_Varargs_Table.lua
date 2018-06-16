function createtable(...)
	return {...}
end

local t = createtable("foo", "bar")
assert(t[1] == "foo", "t - foo")
assert(t[2] == "bar", "t - bar")

-- Not the end
function createtable2(...)
	return {..., "baz"}
end

local t2 = createtable2("foo", "bar")
assert(t2[1] == "foo", "t2 - foo")
assert(t2[2] == "baz", "t2 - baz")

-- Return
function ret(...)
	return ...
end

local t3 = {ret("foo", "bar")}
assert(t3[1] == "foo", "t3 - foo")
assert(t3[2] == "bar", "t3 - bar")