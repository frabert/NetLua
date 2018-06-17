-- Access
assert.Equal(1, math.abs(-1))

-- Overwriting functions
local original = math.abs

math.abs = function()
	return "success"
end

assert.Equal("success", math.abs(-1))
math.abs = original