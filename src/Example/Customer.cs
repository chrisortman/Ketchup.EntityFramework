using System;

namespace Example {
	public class Customer {
		public int ID { get; set; }
		private string _name;
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public DateTime Birthday { get; set; }

		public bool Equals(Customer other) {
			if (ReferenceEquals(null, other)) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			return other.ID == ID;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != typeof (Customer)) {
				return false;
			}
			return Equals((Customer) obj);
		}

		public override int GetHashCode() {
			return ID;
		}
	}
}