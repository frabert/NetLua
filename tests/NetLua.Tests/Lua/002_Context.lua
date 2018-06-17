local foo = "bar"

function test()
	local foo = "baz"
	assert.Equal("baz", foo)
end

test()
assert.Equal("bar", foo)