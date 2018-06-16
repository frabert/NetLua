local foo = "bar"

function test()
	local foo = "baz"
	assert(foo == "baz", "baz")
end

test()
assert(foo == "bar", "bar")