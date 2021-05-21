# OrderSupervisor
This project comprises of an Api and an agent for processing order messages.
The Api is used for enqueuing messages to azure storage queue and store confirmation orders in azure storage table.
Processor is being used to process queue messages and save processed messsage status in confirmation table.

## Required Development Tools

- [Visual Studio 2019](https://www.visualstudio.com/downloads/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET Core 3.1](https://www.microsoft.com/net/download)
- [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)

## Test Steps
- To run the project locally take the latest from git repo and build the solution using VS 2019. 
- Download any missing dependencies using nuget package manager.
- Run the Api project and use it's swagger endpoint POST enqueue message for enqueing order messages.
- Agent.exe can be run from the debug folder path to process the messages in queue.
