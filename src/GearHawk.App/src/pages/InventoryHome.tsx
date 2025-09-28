import { Link, useParams, useNavigate } from 'react-router-dom'
import { useMemo, useState } from 'react'
import { useGetInventoryHomeQuery, type InventoryNodeDto } from '../store/api/gearHawkApi'

const BLOB_BASE = (import.meta as any).env?.VITE_BLOB_BASE_URL || 'https://cs2af83b2550815x4824x9e5.blob.core.windows.net/imagecontainer'
const BLOB_SAS = ((import.meta as any).env?.VITE_BLOB_SAS || '').replace(/^\?/, '')

function withSas(url: string): string {
  if (!BLOB_SAS) return url
  return url.includes('?') ? `${url}&${BLOB_SAS}` : `${url}?${BLOB_SAS}`
}

function imageUrlThumb(node: InventoryNodeDto): string {
  const code = node.CustomerCode || 'img'
  return withSas(`${BLOB_BASE}/${code}_${node.id}_t`)
}

function imageUrlFull(node: InventoryNodeDto): string {
  const code = node.CustomerCode || 'img'
  return withSas(`${BLOB_BASE}/${code}_${node.id}`)
}

export default function InventoryHome() {
  const params = useParams()
  const idParam = params.id ? parseInt(params.id, 10) : 1
  const navigate = useNavigate()
  const { data, isLoading, isError } = useGetInventoryHomeQuery(idParam)

  const root = data?.root
  const containers = useMemo(() => {
    const items = root?.ChildNodesExt ?? []
    return items.filter((n) => n.IsInventory)
  }, [root])

  return (
    <div>
      <div className="inv-header container">
        <h2 className="inv-title">{root?.Name || 'Inventory'}</h2>
      </div>

      {isLoading && (
        <div className="inv-grid container">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="inv-card" style={{ opacity: 0.5 }}>
              <div className="inv-img" />
              <div className="inv-body">
                <div className="inv-name">&nbsp;</div>
                <div className="inv-desc">&nbsp;</div>
                <div className="inv-meta">&nbsp;</div>
              </div>
            </div>
          ))}
        </div>
      )}

      {!isLoading && isError && (
        <div className="container" style={{ padding: 16 }}>
          <p>Failed to load inventory.</p>
        </div>
      )}

      {!isLoading && !isError && (
        <div className="inv-grid container">
          {containers?.length ? (
            containers.map((node) => <ContainerCard key={node.id} node={node} onOpen={() => navigate(`/inventory/${node.id}`)} />)
          ) : (
            <div className="container" style={{ gridColumn: '1 / -1' }}>
              <p>No inventory here yet.</p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

function ContainerCard({ node, onOpen }: { node: InventoryNodeDto; onOpen: () => void }) {
  const qty = parseInt(node.Quantity || '0', 10)
  const min = parseInt(node.MinimumQuantity || '0', 10)
  const low = qty < min
  const [src, setSrc] = useState<string>(imageUrlThumb(node))

  return (
    <button className="inv-card" onClick={onOpen} style={{ textAlign: 'left' }}>
      {/* eslint-disable-next-line jsx-a11y/alt-text */}
      <img
        className="inv-img"
        src={src}
        onError={(e) => {
          const current = (e.currentTarget as HTMLImageElement).src
          if (current === imageUrlThumb(node)) {
            setSrc(imageUrlFull(node))
          } else if (current === imageUrlFull(node)) {
            setSrc(withSas(`${BLOB_BASE}/needsImgB_t.jpg`))
          }
        }}
      />
      <div className="inv-body">
        <h3 className="inv-name">{node.Name}</h3>
        {node.Description && <p className="inv-desc">{node.Description}</p>}
        <div className="inv-meta">
          {node.OutOfService && <span className="badge danger">Out of service</span>}
          {node.NeedsRepair && <span className="badge danger">Needs repair</span>}
          {node.NeedsResupply && <span className="badge warn">Needs resupply</span>}
          <span className={`badge ${low ? 'danger' : ''}`}>qty {qty}/{min}</span>
        </div>
      </div>
    </button>
  )
}


