local str = "'" .. [[
hello
]] .. "'"

assert.Equal("'hello\n'", str)