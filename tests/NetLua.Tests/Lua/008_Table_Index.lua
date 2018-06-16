local t = { "foo", "bar" }
assert(t[1] == "foo", "index at one")

t = { [0] = "foo", "bar" }
assert(t[0] == "foo", "index at zero")