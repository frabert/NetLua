-- Access
assert(math.abs(-1) == 1, "call")

-- Overwriting functions
math.abs = function()
	return "success"
end

assert(math.abs(-1) == "success", "overwriting")