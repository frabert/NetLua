-- Access
assert.Equal(math.abs(-1), 1)

-- Overwriting functions
local original = math.abs

math.abs = function()
	return "success"
end

assert.Equal(math.abs(-1), "success")
math.abs = original