setmetatable(_G, {
	__index = function()
		return "nope"
	end
})

assert.Equal("nope", a)

a = "yup"

assert.Equal("yup", a)