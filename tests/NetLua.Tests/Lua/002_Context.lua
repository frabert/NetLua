local foo = "bar"

function test()
	local foo = "baz"
	assert.Equal(foo, "baz")
end

test()
assert.Equal(foo, "bar")