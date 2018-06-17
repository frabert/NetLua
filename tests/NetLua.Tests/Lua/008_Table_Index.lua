local t = { "foo", "bar" }
assert.Equal("foo", t[1])

t = { [0] = "foo", "bar" }
assert.Equal("foo", t[0])