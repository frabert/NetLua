-- For-loop
local var = ""

for i = 1, 5 do 
	var = var .. i
end

assert(var == "12345", "loop: " .. var)

-- Inverted for-loop
var = ""

for i = 5, 1, -1 do 
	var = var .. i
end

assert(var == "54321", "revert loop")