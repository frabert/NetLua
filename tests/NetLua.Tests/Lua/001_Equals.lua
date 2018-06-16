-- Numbers
local one = 1

assert.Equal(1, 1)
assert.Equal(1, one)
assert.Equal(-1, -one)
assert.NotEqual(1, 10)

-- Strings
local foo = "foo"

assert.Equal("", "")
assert.Equal(foo, "foo")
assert.Equal("foobar", "foo" .. "bar")
assert.NotEqual("foo", "bar")

-- Boolean
assert.True(true)
assert.False(not false)

-- Tables
local t1 = {}
local t2 = {}

assert.Equal(t1, t1)
assert.NotEqual(t1, t2)