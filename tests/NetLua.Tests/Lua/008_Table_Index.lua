local t = { "foo", "bar" }
assert.Equal(t[1], "foo")

t = { [0] = "foo", "bar" }
assert.Equal(t[0], "foo")