using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx;
using IPioneerReceiverControl.Rx.CustomException;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.ExtensionMethod;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx
{
    public class ReceiverController : IReceiverController
    {
        private readonly TransportLayerType _transportLayerType;
        private readonly TcpClient _tcpClient;
        private readonly SerialPort _serialPort;

        private readonly IObservable<IRawReceiverData> _rawDataObservable;
        private readonly IEnumerable<IReceiverCommandDefinition> _commands;

        private readonly IDisposable _disposableDataReceiver;
        private readonly IDictionary<Guid, IReceiverCommandDefinition>

        public IEnumerable<IReceiverCommandDefinition> Commands { get; }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            IObservable<IRawReceiverData> dataObservable, 
            TcpClient tcpClient) : this(commands, dataObservable)
        {
            _transportLayerType = TransportLayerType.Tcp;
            _tcpClient = tcpClient;
        }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            IObservable<IRawReceiverData> dataObservable, 
            SerialPort serialPort) : this(commands, dataObservable)
        {
            _transportLayerType = TransportLayerType.SerialPort;
            _serialPort = serialPort;
        }

        private ReceiverController(IEnumerable<IReceiverCommandDefinition> commands,
            IObservable<IRawReceiverData> dataObservable)
        {
            _rawDataObservable = dataObservable;
            _commands = commands;

            _disposableDataReceiver = _rawDataObservable
                .Subscribe(res =>
                    {

                    },
                    ex =>
                    {

                    },
                    () =>
                    {

                    });
        }
        

        public async Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(IReceiverCommand command, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public async Task SendReceiverCommandAndForgetAsync(IReceiverCommand command)
        {
            var rawCommand = CreateRawCommand(command);

            await SendAsync(rawCommand);
        }

        private string CreateRawCommand(IReceiverCommand command)
        {
            var commandDefinition = _commands.FirstOrDefault(d => d.Command == command?.KeyValue.Key);

            if (commandDefinition is null)
            {
                throw new PioneerReceiverException($"Unknown command: {command.KeyValue.Key}");
            }

            if (commandDefinition.CommandParameterType != command.KeyValue.Value.GetType())
            {
                throw new PioneerReceiverException($"Wrong command type: '{command.KeyValue.Value.GetType()}'. " +
                                                   $"Expected: '{commandDefinition.ResponseParameterType}'");
            }

            string parameter = null;

            if (command.KeyValue.Value is UpDown direction)
            {
                parameter = direction == UpDown.Down ? "D" : "U";
            }

            if (command.KeyValue.Value is OnOff button)
            {
                parameter = button == OnOff.On ? "O" : "F";
            }

            return commandDefinition.CommandTemplate.WildcardReplace('*', parameter);
        }

        private async Task SendAsync(string rawCommand)
        {
            switch (_transportLayerType)
            {
                case TransportLayerType.Tcp:
                    await _tcpClient.SendCommandAsync(rawCommand);
                    break;
                case TransportLayerType.SerialPort:
                    _serialPort.SendCommand(rawCommand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
