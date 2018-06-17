using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Runtime;
using NetLua.Runtime.Ast;

namespace NetLua.Native.Value.Functions
{
    public class LuaInterpreterFunction : LuaFunction
    {
        private readonly Engine _engine;
        private readonly FunctionDefinition _definition;
        private readonly bool _useParent;

        public LuaInterpreterFunction(Engine engine, FunctionDefinition definition, LuaObject context, bool useParent)
        {
            _engine = engine;
            _definition = definition;
            Context = context;
            _useParent = useParent;
        }

        public LuaObject Context { get; set; }

        public override async Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default)
        {
            var context = new LuaTableFunction(Context, _useParent);

            // Set the arguments.
            var i = 0;

            for (; i < _definition.Arguments.Count; i++)
            {
                context.NewIndexRaw(_definition.Arguments[i].Name, args[i]);
            }

            if (_definition.Varargs)
            {
                context.Varargs = args.Skip(i).ToArray();
            }

            // Execute the statements.
            var state = new LuaState(context);
            await _engine.ExecuteStatement(_definition.Body, state, token);

            return state.FunctionState.ReturnArguments;
        }

        public override object ToObject()
        {
            return this;
        }
    }
}