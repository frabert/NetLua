function ret(...)
	return ...
end

function expect_foo_bar_baz(a1, a2, a3)
	assert.Equal(a1, "foo")
	assert.Equal(a2, "bar")
	assert.Equal(a3, "baz")
end

expect_foo_bar_baz(ret("foo", "bar", "baz"))
expect_foo_bar_baz("foo", ret("bar", "baz"))
expect_foo_bar_baz(ret("foo", "skip"), "bar", "baz")