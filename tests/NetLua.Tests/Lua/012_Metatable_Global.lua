setmetatable(_G, {
	__index = function()
		return "nope"
	end
})

assert.Equal(a, "nope")

a = "yup"

assert.Equal(a, "yup")