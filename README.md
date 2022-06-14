# QBCore

QBCore is short for **Q**uery **B**uilder **Core**. This is an attempt to bring the stateful DS/CDS-based application to a stateless analogy.

## API

### QUERY VARIABLES

- `_de`	- Soft delete mode if any
- `_fl`	- Filter
- `_so`	- Sort order
- `_ps`	- Page size
- `_pn`	- Page number
- `_dp`	- Process document field dependencies on the server side (insert or update is not performed in this case)


### DATASOURCE ROUTING

Path names are DS names.

#### Example data models

```
brand { brand_id }
product { product_id, brand_id }
```

#### Example

```
GET, POST                   /api/product
POST                        /api/product?_dp=1
GET, PUT, DELETE, PATCH     /api/product/{product_id}
PUT                         /api/product/{product_id}?_dp=1
GET                         /api/product/filter/brand_id/brand/
GET                         /api/product/filter/brand_id/brand/{brand_id}
GET                         /api/product/cell/brand_id/brand/?_fl=product_id={product_id};depend1={depend1};depend2={depend2};...
GET                         /api/product/cell/brand_id/brand/{brand_id}
```

### COMPLEX DATASOURCE ROUTING

Pathparts are the names of the CDS nodes, but the first root node is addressed by the name of the CDS itself at the beginning of the path.

#### Example data models

```
store-sales:                                                    // CDS
|
|__ store { store_id, region_id }                               // first root
|    |
|    |__ order { order_id, store_id }
|    |    |
|    |    |__ position { store_id, order_id }
|    |
|    |__ supplier { suplier_id, region_id }
|    |
|    |__ available_carrier { carrier_id, store_id, region_id }  // read-only here
|
|__ other-root { other-root_id }                                // other root
```

#### Example

```
GET, POST                  /api/store-sales
POST                       /api/store-sales?_dp=1
GET                        /api/store-sales/filter/region_id/region
GET                        /api/store-sales/filter/region_id/region/{region_id}
GET                        /api/store-sales/cell/region_id/region?_fl=stock.depend1={depend1};stock.depend2={depend2};...
GET                        /api/store-sales/cell/region_id/region/{region_id}
GET, PUT, DELETE, PATCH    /api/store-sales/{store_id}
PUT                        /api/store-sales/{store_id}?_dp=1
GET, POST                  /api/store-sales/{store_id}/order
POST                       /api/store-sales/{store_id}/order?_dp=1
GET, PUT, DELETE, PATCH    /api/store-sales/{store_id}/order/{order_id}
PUT                        /api/store-sales/{store_id}/order/{order_id}?_dp=1

GET, POST                  /api/store-sales/{store_id}/order/{order_id}/position
POST                       /api/store-sales/{store_id}/order/{order_id}/position?_dp=1
GET, PUT, DELETE, PATCH    /api/store-sales/{store_id}/order/{order_id}/position/{position_id}
PUT                        /api/store-sales/{store_id}/order/{order_id}/position/{position_id}?_dp=1

GET                        /api/store-sales/{store_id}/order/{order_id}/position/filter/carrier_id/carrier?_fl=stock.region_id={region_id}
GET                        /api/store-sales/{store_id}/order/{order_id}/position/filter/carrier_id/carrier/{carrier_id}
GET                        /api/store-sales/{store_id}/order/{order_id}/position/cell/carrier_id/carrier?_fl=stock.region_id={region_id}
GET                        /api/store-sales/{store_id}/order/{order_id}/position/cell/carrier_id/carrier/{carrier_id}

GET, POST                  /api/store-sales/_/other-root
POST                       /api/store-sales/_/other-root?_dp=1
GET, PUT, DELETE, PATCH    /api/store-sales/_/other-root/{other-root_id}
PUT                        /api/store-sales/_/other-root/{other-root_id}?_dp=1

GET                        /api/store-sales/_/supplier?_fl=stock.region_id={region_id}
POST                       /api/store-sales/_/supplier
POST                       /api/store-sales/_/supplier?_dp=1
GET, PUT, DELETE, PATCH    /api/store-sales/_/supplier/{supplier_id}
PUT                        /api/store-sales/_/supplier/{supplier_id}?_dp=1

GET                        /api/store-sales/{store_id}/available_carrier?_fl=stock.region_id={region_id}
GET                        /api/store-sales/{store_id}/available_carrier/{carrier_id}?_fl=stock.region_id={region_id}
```

#### CRUD

- `GET /api/product` - Read products
- `GET /api/product/{product_id}` - Read a specific product
- `POST /api/product` - Create a product
- `PUT /api/product/{product_id}` - Update a specific product
- `DELETE /api/product/{product_id}` - Delete a specific product
- `PATCH /api/product/{product_id}` - Restore a specific product if DS supports soft delete.

#### Using of query variables

- `GET /api/product?_de=1&_fl=name~tea&_so=created,2;name,1&_ps=20&_pn=1` - Read deleted products whose 'name' contains 'tea', sort descending by 'created' and then ascending by 'name', take the 1st page of 20 rows.

#### Process dependencies (filter-by-self, filter-by-parent)

There are two types of dependencies:

* filter-by-self,
* filter-by-parent.

Both can be

* client-side,
* server-side.

Client-side dependencies are implemented with javascript, while server-side ones require a request to the server.

In filter-by-self dependencies, the dependent field depends on other fields in the document. In filter-by-parent dependencies, the dependent field depends on other fields from its parent documents in the CDS.

- `POST /api/product?_dp=1` - Must be sent by the client to retrieve the fields in the inserted document that depend on other fields in that document after they have been modified by the user. For example, the "product_color" field should be refreshed to the default product color when changing the "product_id" in the order item card for insert.
- `PUT /api/product/{product_id}?_dp=1` - Same as above but for update.

#### Filter for reference field

- `GET /api/product/filter/brand_id/brand/` - Read brands to make a brand filter in the products.
- `GET /api/product/filter/brand_id/brand/{brand_id}` - Read a specified brand for a brand filter in the products.
- `GET /api/store-sales/filter/region_id/region/{region_id}` - Read regions to make a region filter in the stores.
- `GET /api/store-sales/{store_id}/order/{order_id}/position/filter/carrier_id/carrier?_fl=stock.region_id={region_id}` - Read carriers to make a carrier filter in the carriers. This filter has constraints from the parent nodes on the 'store_id' and 'region_id' fields.

#### List for reference field (filter-by-self, filter-by-parent)

- `GET /api/product/cell/brand_id/brand/?_fl=product_id={product_id};depend1={depend1};depend2={depend2};...` - Read brands to make a list (to choose a brand) in the product card for insert or update. The client must send current values of other document fields on which it depends (filter-by-self) or put them in the path (filter-by-parent) as below
- `GET /api/store-sales/{store_id}/order/{order_id}/position/cell/carrier_id/carrier?_fl=stock.region_id={region_id}` - 'store_id' will be taken from the path and 'region_id' from the filter.

#### Underscore instead of Id

- `GET /api/store-sales/_/other-root` - The underscore character is used instead of an Id when the two nodes in the path are not related.