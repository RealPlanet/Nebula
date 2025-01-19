
# NEBULA - A **scripting** language embeddable in any C++ Program

> To be more specific, nebula is compiled into its own *bytecode* which can then be execute at any item through the __'Nebula Interpreter'__.

This repository contains the following main projects:
- Nebula.Compiler: Console application to link and compile one or more 'Nebula' scripts
- Nebula.Executor: Console application able to execute compiled 'Nebulaì scripts

The rest are:
- Nebula.Commons & Nebula.Shared: Contain shared dependencies for other C# and C++ projects
- Nebula.Core, Nebula.Emitter: Contain the main compiler logic to translate scripts into bytecode
- Nebula.Interop: Contain shared code for C++ and C# as a C++/CLI project. Mainly defines common constants to avoid misalignment issues.

## Every file is a namespace
In Nebula every file has its own namespace. If none is provided the file name is used. This means every file, or better named, 'scripts' does not need to worry about naming
conflicts with other 'scripts'.

A file can define it's own namespace and reference functions and/or bundles from other namespaces.

## Native functions must be imported
A script must import intentionally any native function. This allows for the implementation of functions with similar names without causing compilation errors or shadowing.

A native function is expected to handle the data stack on its own. Mainly, popping the arguments and pushing the return value to it.

## Examples

### Threads can be created at any moment
'Threads' can be created on the fly. With the async keyword any function can be executed without blocking the caller execution.
The 'threads' are actually ran on the same thread of the interpreter and are periodically executed by applying a round-robin strategy.
When a threaded function returns the return type is ignored and any return data is discarded from the stack.

![image thread_example](https://RealPlanet.github.io/assets/ref/nebula/example_async.png)
### 'Bundles' can be defined to group together data

'Bundles' are the 'class' equivalent of Nebula. All primitive data is copied and Bundle data is passed around by reference.

![image var_example](https://RealPlanet.github.io/assets/ref/nebula/example_structs.png)

## What nebula implements currently (in no particular order):

| Feature                                              | Is implemented   |
| -----------                                          |---------         |
| Built-in standard library*                           | ❌               |
| Debug symbols for runtime debugging\*                 | 🟨                 |
| Single-thread async routines                         | ✅               |
| Threads can sleep any amount of time                 | ✅               |
| Binding to native functions for function calls       | ✅               |
| Standard control flow keywords                       | ✅               |
| Floating point data                                  | ❌               |
| Namespaces                                           | ✅               |

\* Due to the nature of the language each implementation must define its own native bindings.
To avoid forcing the hand of the implementer none are required.

\* An initial implementation through the Debug Adapter Protocol has been commited. A VSCode extension will be released eventually. 
![image example_dbg](https://RealPlanet.github.io/assets/ref/nebula/example_dbg_1.png)
## Thanks
A big thanks to [Immo Landwerth](https://www.youtube.com/@ImmoLandwerth), this project initially started from his youtube series as a custom language compiled to IL
but quickly evolved to be something more.