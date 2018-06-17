function ret(...)
	return ...
end

function expect_foo_bar_baz(a1, a2, a3)
	assert.Equal("foo", a1)
	assert.Equal("bar", a2)
	assert.Equal("baz", a3)
end

expect_foo_bar_baz(ret("foo", "bar", "baz"))
expect_foo_bar_baz("foo", ret("bar", "baz"))
expect_foo_bar_baz(ret("foo", "skip"), "bar", "baz")