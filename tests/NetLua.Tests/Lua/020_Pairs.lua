local t = {
	[1] = 1,
	[2] = 2,
	[3] = 3
}
local found = {}

for k, v in pairs(t) do 
	found[k] = k == v
end

assert.True(found[1])
assert.True(found[2])
assert.True(found[3])