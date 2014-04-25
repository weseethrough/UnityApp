//get all customers that has  'm' in Code
(from Customer customer in siaqodb
where customer.Code.Contains("m")
select customer).Skip(5).Take(5)