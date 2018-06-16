setmetatable(_G, {
	__index = function()
		return "nope"
	end
})

assert(a == "nope")

a = "test"

assert(a, "test")