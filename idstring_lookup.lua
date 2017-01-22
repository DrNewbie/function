-- Converts idstring keys back to the original string(s) 
-- Example: idstring_lookup( "79991727a2679722" )
-- Returns: "units/payday2/architecture/bnk/bnk_int_deposit_box"
function idstring_lookup( search )
	local result
	search = search:lower()
	if( DB:has( "idstring_lookup", "idstring_lookup" ) ) then
		local file = DB:open( "idstring_lookup", "idstring_lookup" )
		local data = file:read()   
		for _,text in pairs( string.split( data, '%z' ) ) do
			local key = text:id():key()
			key = key:lower()
			if key == search then
				result = text
				break
			end
		end
		file:close()
	end 
	return result
end