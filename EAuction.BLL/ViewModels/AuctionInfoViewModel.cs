using EAuction.Core.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EAuction.BLL.ViewModels
{
    public class AuctionInfoViewModel
    {
        public string AuctionId { get; set; }
        public string Status { get; set; }
        public string AuctionType { get; set; }
        public string OrganizationName { get; set; }
        public string Description { get; set; }
        public string MinRatingForParticipant { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingConditions { get; set; }
        public decimal StartPrice { get; set; }
        public decimal PriceStep { get; set; }
        public decimal MinPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public DateTime? FinishDateAtActual { get; set; }
        public List<AuctionFile> AuctionFiles { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}\nСтатус: {1}\nТип аукциона: {2}\nНазвание организации: {3}\n" +
                "Описание: {4}\nМинимальный рейтинг для участника: {5}\nАдресс доставки: {6}\n" +
                "Условия доставки :{7}\nНачальная цена :{8}\nШаг :{9}\nМинимальная цена :{10}\nДата начала :{11}\n" +
                "Дата окончания :{12}\nФактическая дата окончания :{13}\n",
                AuctionId, Status, AuctionType, OrganizationName, Description, MinRatingForParticipant,
                ShippingAddress, ShippingConditions, StartPrice, PriceStep, MinPrice,StartDate, FinishDate, FinishDateAtActual);
        }
        
    }

}