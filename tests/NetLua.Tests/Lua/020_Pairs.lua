local t = {
	a = 1,
	b = 2,
	c = 3
}
local str = ""

for k, v in pairs(t) do 
	str = str .. k .. v
end

assert.Equal("a1b2c3", str)