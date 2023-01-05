Create procedure GetSumAndMediane
	@sum bigint out,
	@mediane float out
As
Begin
SELECT @sum= SUM(Element4) FROM Task1Output;

select @mediane = avg(Element5)
from
( Select *, ROW_NUMBER() over (order by Element5 desc) as desc_El5,
		ROW_NUMBER() over (order by Element5 asc) as asc_El5
		From Task1Output ) as a
where asc_El5 in (desc_El5, desc_El5+1, desc_El5-1)

End
