using System;
using System.Collections.Generic;
using static Order;

public class Store
{
	public List<Product> Products { get; set; }
	public List<Order> Orders { get; set; }
	public List<YearStatics> InformationOnYears { get; set; }

	/// <summary>
	/// Формирует строку со статистикой продаж продуктов
	/// Сортировка - по убыванию кол-ва проданных продуктов
	/// </summary>
	/// <param name="year">Год, за который подсчитывается статистика</param>
	public string GetProductStatistics(int year)
	{
		YearStatics yearStatics = GenerateYearsStatistics(year);

		string result = "";
		for (int i = 0; i < yearStatics.Items.Count; i++)
		{
			result += i+1 + ") " + yearStatics.Items[i].NameProduct + " - " + yearStatics.Items[i].Quantity + " item(s)\n";
		}


		return result;
	}

	public YearStatics GenerateYearsStatistics(int year)
    {
		YearStatics yearStatics = new YearStatics();
		yearStatics.Year = year;	
		yearStatics.Items = new List<ProductStatics>();
		List<Order> FindOrders = Orders.FindAll(i => i.OrderDate.Year == year);
		foreach (var order in FindOrders)
		{
			foreach (OrderItem item in order.Items)
			{
				var findProduct = yearStatics.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
				if (findProduct != null)
				{
					findProduct.Quantity = findProduct.Quantity + item.Quantity;
				}
				else
				{
					var ProductData = Products.FirstOrDefault(i => i.Id == item.ProductId);
					if (ProductData != null)
					{
						ProductStatics product = new ProductStatics();
						product.ProductId = item.ProductId;
						product.NameProduct = ProductData.Name;
						product.Price = ProductData.Price;
						product.Quantity = item.Quantity;

						yearStatics.Items.Add(product);
					}
				}
			}
		}

		return yearStatics;
	}

	/// <summary>
	/// Формирует строку со статистикой продаж продуктов по годам
	/// Сортировка - по убыванию годов.
	/// Выводятся все года, в которых были продажи продуктов
	/// </summary>
	public string GetYearsStatistics()
	{
		if (Orders.Count == 0) { return string.Empty; }
		string result = "";
		List<int> YearList = new List<int>();
		InformationOnYears = new List<YearStatics>();
		YearList.Add(Orders[0].OrderDate.Year);
		foreach (Order order in Orders)
		{
			for (int i = 0; i < YearList.Count; i++)
			{
				if (order.OrderDate.Year != YearList[i])
				{
					YearList.Add(order.OrderDate.Year);
				}
			}
		}

		foreach (int time in YearList)
        {
			InformationOnYears.Add(GenerateYearsStatistics(time));
        }

		foreach(YearStatics statics in InformationOnYears)
        {
			result += statics.Year + " - " + statics.Revenue + " руб.\n" + "Most selling: " + statics.MostSellingItem.NameProduct + " (" + statics.MostSellingItem.Quantity + " item(s))\r\n";
        }

		// Формат результата:
		// {Год} - {На какую сумму продано продуктов руб\r\n
		// Most selling: -{Название самого продаваемого продукта} (кол-во проданных единиц самого популярного продукта шт.)\r\n
		// \r\n
		//
		// Пример:
		//
		// 2021 - 630.000 руб.
		// Most selling: Product 1 (380 item(s))
		//
		// 2020 - 630.000 руб.
		// Most selling: Product 1 (380 item(s))
		//
		// 2019 - 130.000 руб.
		// Most selling: Product 3 (10 item(s))
		//
		// 2018 - 50.000 руб.
		// Most selling: Product 3 (5 item(s))

		// TODO Реализовать логику получения и формирования требуемых данных        

		return result;
	}
}

public class Product
{
	public int Id { get; set; }
	public string Name { get; set; }
	public double Price { get; set; }
}

public class YearStatics
{
	public int Year { get; set; }
	public List<ProductStatics> Items { get; set; }
	public double Revenue 
	{ 
		get
        {
			double revenue = 0;
			foreach(ProductStatics item in Items)
            {
				revenue += item.Revenue;
            }
			return revenue;
        } 
	}
	public ProductStatics MostSellingItem
    {
        get
        {
			if (Items == null)
				return null;

			ProductStatics MostSellingProduct = new ProductStatics();
			MostSellingProduct = Items[0];
			foreach(ProductStatics item in Items)
            {
				if (item.Quantity > MostSellingProduct.Quantity)
                {
					MostSellingProduct = item;
				}
            }

			return MostSellingProduct;

		}
    }
}

public class ProductStatics
{
	public int ProductId { get; set; }
	public string NameProduct { get; set; }
	public int Quantity { get; set; }
	public double Revenue { get => Quantity * Price; }
    public double Price { get; set; }
}

public class Order
{
	public int UserId { get; set; }
	public List<OrderItem> Items { get; set; }
	public DateTime OrderDate { get; set; }

	public class OrderItem
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; }
	}
}

public class Program
{
	static void Main(string[] args)
	{
		// ПРИМЕР того, каким образом мы будем заполнять коллекции
		// НЕ является тестовым примером
		var store = new Store
		{
			Products = new List<Product>
			{
				new() { Id = 1, Name = "Product 1", Price = 1000d },
				new() { Id = 2, Name = "Product 2", Price = 3000d },
				new() { Id = 3, Name = "Product 3", Price = 10000d }
			},
			Orders = new List<Order>
			{
				new()
				{
					UserId = 1,
					OrderDate = DateTime.UtcNow,
					Items = new List<Order.OrderItem>
					{
						new() { ProductId = 1, Quantity = 2 }
					}
				},
				new()
				{
					UserId = 1,
					OrderDate = DateTime.UtcNow,
					Items = new List<Order.OrderItem>
					{
						new() { ProductId = 1, Quantity = 1 },
						new() { ProductId = 2, Quantity = 1 },
						new() { ProductId = 3, Quantity = 1 }
					}
				}
			}
		};

		Console.WriteLine(store.GetProductStatistics(2022));
		Console.WriteLine(store.GetYearsStatistics());

		//Насоздавал своих моделей, а компораторы написать забыл... Без сортировки получается...
	}
}