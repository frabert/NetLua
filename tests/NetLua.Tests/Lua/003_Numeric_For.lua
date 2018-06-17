-- For-loop
local var = ""

for i = 1, 5 do 
	var = var .. i
end

assert.Equal("12345", var)

-- Inverted for-loop
var = ""

for i = 5, 1, -1 do 
	var = var .. i
end

assert.Equal("54321", var)