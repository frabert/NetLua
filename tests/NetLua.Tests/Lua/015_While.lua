local i = 1
local a = {true, true, false}

while a[i] do
    i = i + 1
end

assert.Equal(3, i)