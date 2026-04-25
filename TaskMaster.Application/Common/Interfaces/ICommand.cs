using MediatR;

namespace TaskMaster.Application.Common.Interfaces;

// Para comandos que devuelven un valor (ej: el ID creado)
public interface ICommand<out TResponse> : IRequest<TResponse> { }

// Para comandos que no devuelven nada (Void/Unit)
public interface ICommand : IRequest { }
