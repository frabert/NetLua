function ret(...)
	return ...
end

function expect_foo_bar_baz(a1, a2, a3)
	assert(a1 == "foo", "a1 - foo")
	assert(a2 == "bar", "a2 - bar")
	assert(a3 == "baz", "a3 - baz")
end

expect_foo_bar_baz(ret("foo", "bar", "baz"))
expect_foo_bar_baz("foo", ret("bar", "baz"))
expect_foo_bar_baz(ret("foo", "skip"), "bar", "baz")