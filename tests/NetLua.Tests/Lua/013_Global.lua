-- global
assert.Equal(_G.test, nil)

test = true

assert.True(_G.test)

-- Func > global
function func()
	test = false
end

func()
assert.False(_G.test)

-- Func > local
function func2()
	local test = true
end

func2()
assert.False(_G.test)

-- Reset
test = nil