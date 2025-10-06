-- SELECT "StaffId" , COUNT("StaffId") AS "STAFFID_TOTAL" FROM public."Users" GROUP BY "StaffId" ;

select staffid , COUNT(staffid) as "staffid_total" from public."Users" u group by staffid ;