using System;

namespace Example {
	public class Customer {
		public int Id { get; set; }
		private string _name;
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public DateTime Birthday { get; set; }
	}
}