from Customer customer in siaqodb
	where customer.Code=="BOLID"
	join Invoice invoice in siaqodb
	on customer.OID equals invoice.CustomerOID
	select new { Customer = customer.Code, InvoiceNumber = invoice.Number, InvoiceTotal = invoice.Total }