local t = {"a", "b", "c"}
local str = ""

for i, v in ipairs(t) do 
	str = str .. v
end

assert.Equal("abc", str)