# GearHawk.App

A modern React application built with Vite, TypeScript, and Redux Toolkit for state management. This app is designed to work with the GearHawk API and Microsoft Entra authentication.

## Features

- ⚡ **Vite** - Fast development and build tool
- 🔷 **TypeScript** - Type safety and better developer experience
- 🛠️ **Redux Toolkit** - Modern Redux with simplified state management
- 🔄 **RTK Query** - Powerful API caching and data fetching
- 🛣️ **React Router** - Client-side routing
- 🔐 **Microsoft Entra** - Enterprise authentication ready
- 📡 **Axios** - HTTP client for API calls
- 💾 **Persistent State** - Authentication state persists across sessions

## Getting Started

### Prerequisites

- Node.js (v18 or higher)
- npm or yarn

### Installation

1. Install dependencies:
```bash
npm install
```

2. Create a `.env` file in the root directory with your Microsoft Entra configuration:
```env
# Microsoft Entra (Azure AD) Configuration
VITE_AZURE_CLIENT_ID=your-client-id-here
VITE_AZURE_TENANT_ID=your-tenant-id-here
VITE_REDIRECT_URI=http://localhost:5173
VITE_POST_LOGOUT_REDIRECT_URI=http://localhost:5173

# API Configuration
VITE_API_BASE_URL=https://localhost:7001
```

3. Start the development server:
```bash
npm run dev
```

The app will be available at `http://localhost:5173`

## Project Structure

```
src/
├── components/          # Reusable React components
├── store/              # Redux store and slices
│   ├── api/            # RTK Query API definitions
│   │   └── gearHawkApi.ts
│   ├── slices/         # Redux Toolkit slices
│   │   ├── authSlice.ts
│   │   └── counterSlice.ts
│   ├── hooks.ts        # Typed Redux hooks
│   └── index.ts        # Store configuration
├── config/             # Configuration files
│   └── authConfig.ts   # Microsoft Entra configuration
├── services/           # Additional API services
├── types/              # TypeScript type definitions
├── utils/              # Utility functions
├── App.tsx             # Main application component
└── main.tsx            # Application entry point
```

## State Management with Redux Toolkit

This project uses **Redux Toolkit** and **RTK Query** for state management and API caching:

### **Redux Toolkit Benefits:**
- ✅ **Simplified Redux** - Less boilerplate than traditional Redux
- ✅ **Built-in Immer** - Immutable updates with mutable syntax
- ✅ **DevTools Integration** - Excellent debugging experience
- ✅ **TypeScript Support** - Full type safety
- ✅ **RTK Query** - Built-in API caching and data fetching

### **RTK Query Benefits:**
- ✅ **Automatic Caching** - Smart cache invalidation
- ✅ **Background Updates** - Keep data fresh
- ✅ **Optimistic Updates** - Instant UI feedback
- ✅ **Request Deduplication** - No duplicate requests
- ✅ **Loading States** - Built-in loading/error states

### Example Store Usage

```typescript
import { useAppDispatch, useAppSelector } from './store/hooks'
import { increment, decrement } from './store/slices/counterSlice'

function MyComponent() {
  const dispatch = useAppDispatch()
  const { count } = useAppSelector((state) => state.counter)
  
  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={() => dispatch(increment())}>+</button>
      <button onClick={() => dispatch(decrement())}>-</button>
    </div>
  )
}
```

### Example API Usage with RTK Query

```typescript
import { useGetInventoryQuery, useCreateInventoryItemMutation } from './store/api/gearHawkApi'

function InventoryComponent() {
  const { data: inventory, isLoading, error } = useGetInventoryQuery()
  const [createItem, { isLoading: isCreating }] = useCreateInventoryItemMutation()
  
  const handleCreate = async (item) => {
    try {
      await createItem(item).unwrap()
      // Cache is automatically updated!
    } catch (error) {
      console.error('Failed to create item:', error)
    }
  }
  
  if (isLoading) return <div>Loading...</div>
  if (error) return <div>Error: {error.message}</div>
  
  return (
    <div>
      {inventory?.map(item => (
        <div key={item.id}>{item.name}</div>
      ))}
      <button onClick={() => handleCreate({ name: 'New Item' })}>
        Add Item
      </button>
    </div>
  )
}
```

## Microsoft Entra Integration

The app is configured for Microsoft Entra (Azure AD) authentication:
- Configuration file ready (`src/config/authConfig.ts`)
- Authentication slice with persistence
- Environment variables template
- MSAL libraries installed

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Development

### Adding New Features

1. **State Management**: Create new slices in `src/store/slices/`
2. **API Calls**: Add endpoints to `src/store/api/gearHawkApi.ts`
3. **Components**: Add reusable components in `src/components/`
4. **Routing**: Add new routes in `App.tsx`

### Best Practices

- Use TypeScript for all new code
- Follow the existing slice patterns for state management
- Use RTK Query for all API calls
- Use the provided authentication slice for user state
- Keep components small and focused
- Use environment variables for configuration

## Deployment

1. Build the application:
```bash
npm run build
```

2. The built files will be in the `dist/` directory
3. Deploy the contents of `dist/` to your web server

## Contributing

1. Follow the existing code style
2. Add TypeScript types for new features
3. Update this README for significant changes
4. Test your changes thoroughly

## License

This project is part of the GearHawk solution.
