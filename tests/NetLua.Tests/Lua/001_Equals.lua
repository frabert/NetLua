-- Numbers
local one = 1
assert(1 == 1, "same numbers")
assert(1 == one, "same number and var")
assert(1 ~= 10, "different numbers")
assert(-1 == -one, "unary variable")

-- Strings
local foo = "foo"
assert("" == "", "empty string")
assert(foo == "foo", "same var and string")
assert("foo" ~= "bar", "different strings")
assert("foobar" == "foo" .. "bar", "string concat")

-- Boolean
assert(true, "true")
assert(not false, "not false")

-- Tables
local t1 = {}
local t2 = {}

assert(t1 == t1, "same tables")
assert(t1 ~= t2, "different tables")