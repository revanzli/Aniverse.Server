﻿using Aniverse.MessageContracts;
using Aniverse.MessageContracts.Events.Message;
using ChatService.API.DataAccess.Entities;
using ChatService.API.DTOs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.API.Hubs
{
    public class ChatHub : Hub
    {
        readonly ISendEndpointProvider _sendEndpointProvider;
        public ChatHub(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }
        public async Task SendMessage(SendMessageDto messageDto)
        {
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new($"queue:{RabbitMqConstants.StateMachine}"));
            SendMessageEvent sendMessage = new()
            {
                Message = messageDto.Content,
                SenderUserId = messageDto.SenderUserId,
                ReceiverUserId = messageDto.ReceiverUserId,
                SenderDate = DateTime.UtcNow,
            };
            await sendEndpoint.Send<SendMessageEvent>(sendMessage);
            await Clients.User(messageDto.ReceiverUserId).SendAsync("receiveMessage", messageDto);
        }
        public async Task SendMessageAll(string message)
        {
            await Clients.All.SendAsync(message);
        }
    }
}
