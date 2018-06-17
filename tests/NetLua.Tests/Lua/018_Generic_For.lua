function counter(limit, current)
	current = current or 0

	if current >= limit then
		return nil
	end

	return current + 1
end

local str = ""

for c in counter, 5 do
	str = str .. c
end

assert.Equal("12345", str)