# QBCore

QBCore is short for **Q**uery **B**uilder **Core**. This is an attempt to bring the stateful DS/CDS-based application to a stateless analogy. It is an R&D project aimed at developing a platform for business process automation that allows writing code in a more descriptive way, but less than low-code platforms do. Such an approach would avoid the limitations that low-code and no-code platforms have, but could significantly increase the speed of developing solutions based on it. A challenging part is developing a generic query builder layer that can work with both relational and document databases, and allows using EF, MongoDB C#/.NET Driver, or just .NET Data Provider.

## API

### QUERY VARIABLES

- `mode`	- Soft delete mode if any
- `filter`	- Filter
- `sort`	- Sort order
- `psize`	- Page size
- `pnum`	- Page number
- `depends`	- Process document field dependencies on the server side (insert or update is not performed in this case)


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
POST                        /api/product?depends=1
GET, PUT, DELETE, PATCH     /api/product/{product_id}
PUT                         /api/product/{product_id}?depends=1
GET                         /api/product/filter/brand_id/brand/
GET                         /api/product/filter/brand_id/brand/{brand_id}
GET                         /api/product/cell/brand_id/brand/?filter=product_id={product_id};depend1={depend1};depend2={depend2};...
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
POST                       /api/store-sales?depends=1
GET                        /api/store-sales/filter/region_id/region
GET                        /api/store-sales/filter/region_id/region/{region_id}
GET                        /api/store-sales/cell/region_id/region?filter=stock.depend1={depend1};stock.depend2={depend2};...
GET                        /api/store-sales/cell/region_id/region/{region_id}
GET, PUT, DELETE, PATCH    /api/store-sales/{store_id}
PUT                        /api/store-sales/{store_id}?depends=1
GET, POST                  /api/store-sales/{store_id}/order
POST                       /api/store-sales/{store_id}/order?depends=1
GET, PUT, DELETE, PATCH    /api/store-sales/{store_id}/order/{order_id}
PUT                        /api/store-sales/{store_id}/order/{order_id}?depends=1

GET, POST                  /api/store-sales/{store_id}/order/{order_id}/position
POST                       /api/store-sales/{store_id}/order/{order_id}/position?depends=1
GET, PUT, DELETE, PATCH    /api/store-sales/{store_id}/order/{order_id}/position/{position_id}
PUT                        /api/store-sales/{store_id}/order/{order_id}/position/{position_id}?depends=1

GET                        /api/store-sales/{store_id}/order/{order_id}/position/filter/carrier_id/carrier?filter=stock.region_id={region_id}
GET                        /api/store-sales/{store_id}/order/{order_id}/position/filter/carrier_id/carrier/{carrier_id}
GET                        /api/store-sales/{store_id}/order/{order_id}/position/cell/carrier_id/carrier?filter=stock.region_id={region_id}
GET                        /api/store-sales/{store_id}/order/{order_id}/position/cell/carrier_id/carrier/{carrier_id}

GET, POST                  /api/store-sales/_/other-root
POST                       /api/store-sales/_/other-root?depends=1
GET, PUT, DELETE, PATCH    /api/store-sales/_/other-root/{other-root_id}
PUT                        /api/store-sales/_/other-root/{other-root_id}?depends=1

GET                        /api/store-sales/_/supplier?filter=stock.region_id={region_id}
POST                       /api/store-sales/_/supplier
POST                       /api/store-sales/_/supplier?depends=1
GET, PUT, DELETE, PATCH    /api/store-sales/_/supplier/{supplier_id}
PUT                        /api/store-sales/_/supplier/{supplier_id}?depends=1

GET                        /api/store-sales/{store_id}/available_carrier?filter=stock.region_id={region_id}
GET                        /api/store-sales/{store_id}/available_carrier/{carrier_id}?filter=stock.region_id={region_id}
```

#### CRUD

- `GET /api/product` - Read products
- `GET /api/product/{product_id}` - Read a specific product
- `POST /api/product` - Create a product
- `PUT /api/product/{product_id}` - Update a specific product
- `DELETE /api/product/{product_id}` - Delete a specific product
- `PATCH /api/product/{product_id}` - Restore a specific product if DS supports soft delete.

#### Using of query variables

- `GET /api/product?mode=1&filter=name~tea&sort=created,2;name,1&psize=20&pnum=1` - Read deleted products whose 'name' contains 'tea', sort descending by 'created' and then ascending by 'name', take the 1st page of 20 rows.

#### Process dependencies (filter-by-self, filter-by-parent)

There are two types of dependencies:

* filter-by-self,
* filter-by-parent.

Both can be

* client-side,
* server-side.

Client-side dependencies are implemented with javascript, while server-side ones require a request to the server.

In filter-by-self dependencies, the dependent field depends on other fields in the document. In filter-by-parent dependencies, the dependent field depends on other fields from its parent documents in the CDS.

- `POST /api/product?depends=1` - Must be sent by the client to retrieve the fields in the inserted document that depend on other fields in that document after they have been modified by the user. For example, the "product_color" field should be refreshed to the default product color when changing the "product_id" in the order item card for insert.
- `PUT /api/product/{product_id}?depends=1` - Same as above but for update.

#### Filter for reference field

- `GET /api/product/filter/brand_id/brand/` - Read brands to make a brand filter in the products.
- `GET /api/product/filter/brand_id/brand/{brand_id}` - Read a specified brand for a brand filter in the products.
- `GET /api/store-sales/filter/region_id/region/{region_id}` - Read regions to make a region filter in the stores.
- `GET /api/store-sales/{store_id}/order/{order_id}/position/filter/carrier_id/carrier?filter=stock.region_id={region_id}` - Read carriers to make a carrier filter in the carriers. This filter has constraints from the parent nodes on the 'store_id' and 'region_id' fields.

#### List for reference field (filter-by-self, filter-by-parent)

- `GET /api/product/cell/brand_id/brand/?filter=product_id={product_id};depend1={depend1};depend2={depend2};...` - Read brands to make a list (to choose a brand) in the product card for insert or update. The client must send current values of other document fields on which it depends (filter-by-self) or put them in the path (filter-by-parent) as below
- `GET /api/store-sales/{store_id}/order/{order_id}/position/cell/carrier_id/carrier?filter=stock.region_id={region_id}` - 'store_id' will be taken from the path and 'region_id' from the filter.

#### Underscore instead of Id

- `GET /api/store-sales/_/other-root` - The underscore character is used instead of an Id when the two nodes in the path are not related.


# QBCore Develop
QBCore.Develop is an application for developing QBCore applications. The main idea is to generate application source code (entities, their models, data sources and complex data sources, etc.) based on data taken from Develop. The QBCore.Develop application is also developed in itself.

## Relational Object Model

						Projects			Languages
						/	|					\
					Apps -- | --------------- Translations
					/		|						|
		FuncGroupsByApps  	|						|
					\		|						|
					FuncGroups						|
					/		\						|
		GenericObjects -- AppObjects (DS, CDS) -----|
			|										|
			|-- DataEntries ------------------------|
			|
			|-- CDSNodes ---------------------------|
			|	|-- CDSConditions
			|
			|-- AOListeners
			|
			|-- QueryBuilders
			|	|-- QBObjects
			|	|-- QBColumns
			|	|-- QBParameters
			|	|-- QBJoinConditions
			|	|-- QBConditions
			|	|-- QBSortOrders
			|	|-- QBAggregations
			|
			|-- Other


# CLI & other
#### PostgreSQL
- `docker pull postgres`
- `docker run -d --name pgsql -e POSTGRES_USER=user1 -e POSTGRES_PASSWORD=Pass#word1 -p 5432:5432 -v /data:/var/lib/postgresql/data postgres`
- `docker start pgsql`

#### PgAdmin 4
- `docker pull dpage/pgadmin4`
- `docker run -d --name pgadmin -p 82:80 -e 'PGADMIN_DEFAULT_EMAIL=user@domain.com' -e 'PGADMIN_DEFAULT_PASSWORD=Password1' dpage/pgadmin4`
- `docker inspect pgsql -f “{{json .NetworkSettings.Networks }}”`
- `docker start pgadmin`

#### Set user-secrets
- `dotnet user-secrets init`
- `dotnet user-secrets set SqlDbSettings:Password Pass#word1`

#### Mongo
- `docker run -d --name mongo -p 27017:27017 -v mongodbdata:/data/db -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=Pass#word1 mongo:5.0.8`
- `docker exec -it mongo mongosh "mongodb://mongoadmin:Pass%23word1@127.0.0.1:27017/?authSource=admin&readPreference=primary&ssl=false"`
