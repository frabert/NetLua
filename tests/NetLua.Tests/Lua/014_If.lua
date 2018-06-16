local result

-- Basic
if 5 < 10 then
	result = true
else
	result = false
end

assert.True(result)

-- ElseIf
if 5 > 10 then
	result = false
elseif nil then
	result = false
elseif true then
	result = true
else
	result = false
end

assert.True(result)